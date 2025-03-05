using System.Diagnostics;
using System.Text;

class State {

    public State() {
        rowByColumn = new int[SIZE];
    }

    public int Column { get { return column; } }
    public IEnumerable<int> Rows() {
        // TODO: use lookup table
        byte maskedRows = (byte) (rows | (d1 >> column) | (d2 >> (SIZE - column - 1)));
        byte mask = 1;
        for (int i = 0; i < SIZE; i++) {
            if ((maskedRows & mask) == 0) {
                yield return i;
            }

            mask <<= 1;
        }
    }

    public void SetRow(int row) {
        rowByColumn[column] = row;
        Debug.Assert((rows & RowMask(row)) == 0);
        Debug.Assert((d1 & D1Mask(row, column)) == 0);
        Debug.Assert((d2 & D2Mask(row, column)) == 0);
        rows = (byte)(rows | (RowMask(row)));
        d1 = (ushort)(d1 | (D1Mask(row, column)));
        d2 = (ushort)(d2 | (D2Mask(row, column)));
        Debug.Assert((rows & RowMask(row)) != 0);
        Debug.Assert((d1 & D1Mask(row, column)) != 0);
        Debug.Assert((d2 & D2Mask(row, column)) != 0);
        column += 1;
    }
    public void Undo() {
        var prevColumn = this.column - 1;
        var row = rowByColumn[prevColumn];
        Debug.Assert((rows & RowMask(row)) != 0);
        Debug.Assert((d1 & D1Mask(row, prevColumn)) != 0);
        Debug.Assert((d2 & D2Mask(row, prevColumn)) != 0);
        rows = (byte) (rows ^ RowMask(row));
        d1 = (ushort) (d1 ^ (D1Mask(row, prevColumn)));
        d2 = (ushort)(d2 ^ (D2Mask(row, prevColumn)));
        this.column = prevColumn;
    }

    public bool IsDone() {
        return column == SIZE;
    }

    private static int D2Mask(int row, int column) {
        return 1 << (row - column + SIZE - 1);
    }

    private static int D1Mask(int row, int column) {
        return 1 << (row + column);
    }

    private static int RowMask(int row) {
        return 1 << row;
    }

    const int SIZE = 8;

    private int column;
    private readonly int[] rowByColumn;

    // bitfields showing occupied rows/diagonals
    //
    // Given this board position:
    //
    // d2 14
    // |
    // v
    // 00000000 <-- row 7, d1 14
    // 00000000
    // 00000000
    // 00000000
    // 00000000
    // 01000000
    // 00000000
    // 10000000 // row 0, d2 0
    // ^
    // |
    // d1 0
    //
    // rows = 000000101
    // d1 = 000000000001001
    // d2 = 000000110000000


    public override String ToString() {
        var sb = new StringBuilder();
        for (var row = SIZE - 1; row >= 0; row -= 1) {
            for (var column = 0; column < SIZE; column += 1) {
                sb.Append(rowByColumn[column] == row ? '*' : '0');
            }
            sb.Append(Environment.NewLine);
        }

        return sb.ToString();
    }

    private byte rows;
    private ushort d1;
    private ushort d2; // reversed other diagonal

    private static int undos = 0;
    private static bool FindSolution(State state) {
        if (state.IsDone()) {
            return true;
        }

        foreach (var row in state.Rows()) {
            state.SetRow(row);
            if (FindSolution(state)) {
                return true;
            }
            state.Undo();
            undos++;
        }

        return false;
    }

    public static void Main() {
        var state = new State();
        FindSolution(state);
        Console.WriteLine(state.ToString());
        Console.WriteLine(undos);
    }
}
