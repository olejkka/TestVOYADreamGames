namespace _Project.Scripts.Generators
{
    public class Field
    {
        public int Width { get; }
        public int Height { get; }
        public int[] Cells { get; }
        

        public Field(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new int[width * height];
            
            for (int i = 0; i < Cells.Length; i++)
                Cells[i] = -1;
        }
    }
}
