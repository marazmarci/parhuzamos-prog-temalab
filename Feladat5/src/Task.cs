namespace Feladat5 {
    
    public abstract class Task {
        
        public int Color { get; private set; }
        public int ID { get; set; }

        public Task(int color) {
            Color = color;
        }
        
        public abstract void Run();

        public override string ToString() => $"Task#{ID}";
    }
    
}