namespace Feladat5 {
    
    public abstract class Task {
        
        public int Color { get; set; }
        
        public Task(int color) {
            Color = color;
        }
        
        public abstract void run();
    }
    
}