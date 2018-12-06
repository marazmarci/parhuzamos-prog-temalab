using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using static TaskLogEntryType;

namespace Feladat5 {
    
    [TestFixture]
    public class ScheduleValidatorTests {

        private List<TaskLogEntry> log;
        
        public static int BLUE = 1;
        public static int GREEN = 2;
        public static int RED = 3;
        
        private DummyTask blueTask1 = new DummyTask(1, BLUE);
        private DummyTask blueTask2 = new DummyTask(2, BLUE);
        private DummyTask blueTask3 = new DummyTask(3, BLUE);
        private DummyTask greenTask1 = new DummyTask(1, GREEN);
        private DummyTask greenTask2 = new DummyTask(2, GREEN);
        private DummyTask greenTask3 = new DummyTask(3, GREEN);
        private DummyTask redTask1 = new DummyTask(1, RED);
        private DummyTask redTask2 = new DummyTask(2, RED);
        private DummyTask redTask3 = new DummyTask(3, RED);
        
        private Thread thread1 = new Thread(() => { }) { Name = "Consumer#1" };
        private Thread thread2 = new Thread(() => { }) { Name = "Consumer#2" };
        private Thread thread3 = new Thread(() => { }) { Name = "Consumer#3" };


        [Test]
        public void TestRule1() {
            // Egy feladatot csak egy végrehajtónak szabad kiadni.
            
            log.Add(new TaskLogEntry(blueTask1, START, thread1, null));
            log.Add(new TaskLogEntry(redTask1, START, thread2, null));
            
            log.Add(new TaskLogEntry(greenTask1, START, thread3, null));
            log.Add(new TaskLogEntry(greenTask1, FINISH, thread3, null));
            
            log.Add(new TaskLogEntry(blueTask1, FINISH, thread1, null));
            log.Add(new TaskLogEntry(redTask1, FINISH, thread2, null));
            
            Assert.True(ScheduleValidator.IsValid(log));
        }


        [Test]
        public void TestRule1Negative() {
            // Egy feladatot csak egy végrehajtónak szabad kiadni.
            
            log.Add(new TaskLogEntry(blueTask1, START, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, START, thread2, null));
            
            log.Add(new TaskLogEntry(blueTask1, FINISH, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, FINISH, thread2, null));
            
            Assert.False(ScheduleValidator.IsValid(log));
        }


        [Test]
        public void TestRule2() {
            // Ha a feladat végrehajtása valamilyen okból meghiúsul (pl. a végrehajtó szál elesik), a feladatot vissza kell tenni a végrehajtási sorba.
            
            log.Add(new TaskLogEntry(blueTask1, START, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, FAIL, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, START, thread2, null));
            log.Add(new TaskLogEntry(blueTask1, FINISH, thread2, null));
            
            log.Add(new TaskLogEntry(redTask1, START, thread1, null));
            log.Add(new TaskLogEntry(redTask1, FAIL, thread1, null));
            log.Add(new TaskLogEntry(redTask1, START, thread1, null));
            log.Add(new TaskLogEntry(redTask1, FINISH, thread1, null));
            
            Assert.True(ScheduleValidator.IsValid(log));
        }


        [Test]
        public void TestRule2Negative() {
            // Ha a feladat végrehajtása valamilyen okból meghiúsul (pl. a végrehajtó szál elesik), a feladatot vissza kell tenni a végrehajtási sorba.
            
            log.Add(new TaskLogEntry(blueTask1, START, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, FAIL, thread1, null));
            
            log.Add(new TaskLogEntry(redTask1, START, thread1, null));
            log.Add(new TaskLogEntry(redTask1, FINISH, thread1, null));
            
            Assert.False(ScheduleValidator.IsValid(log));
        }


        [Test]
        public void TestRule3() {
            // Az egész rendszerben egy adott színű feladatból egy időben csak egy lehet végrehajtás alatt.
            
            log.Add(new TaskLogEntry(blueTask1, START, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, FINISH, thread1, null));
            
            log.Add(new TaskLogEntry(blueTask2, START, thread1, null));
            
            log.Add(new TaskLogEntry(redTask1, START, thread2, null));
            log.Add(new TaskLogEntry(redTask1, FINISH, thread2, null));
            
            log.Add(new TaskLogEntry(blueTask2, FINISH, thread1, null));
            
            Assert.True(ScheduleValidator.IsValid(log));
        }


        [Test]
        public void TestRule3Negative() {
            // Az egész rendszerben egy adott színű feladatból egy időben csak egy lehet végrehajtás alatt.
            
            log.Add(new TaskLogEntry(blueTask1, START, thread1, null));
            log.Add(new TaskLogEntry(blueTask2, START, thread2, null));
            log.Add(new TaskLogEntry(blueTask1, FINISH, thread1, null));
            log.Add(new TaskLogEntry(blueTask2, FINISH, thread2, null));
            
            Assert.False(ScheduleValidator.IsValid(log));
        }


        [Test]
        public void TestRule4() {
            // Egy adott színhez tartozó feladatokat a beadás sorrendjében kell végrehajtani.
            
            log.Add(new TaskLogEntry(blueTask1, START, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, FINISH, thread1, null));
            log.Add(new TaskLogEntry(blueTask2, START, thread1, null));
            log.Add(new TaskLogEntry(redTask1, START, thread2, null));
            log.Add(new TaskLogEntry(blueTask2, FINISH, thread1, null));
            log.Add(new TaskLogEntry(redTask1, FINISH, thread2, null));
            
            Assert.True(ScheduleValidator.IsValid(log));
        }


        [Test]
        public void TestRule4Negative() {
            // Egy adott színhez tartozó feladatokat a beadás sorrendjében kell végrehajtani.
            
            log.Add(new TaskLogEntry(blueTask2, START, thread1, null));
            log.Add(new TaskLogEntry(blueTask2, FINISH, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, START, thread1, null));
            log.Add(new TaskLogEntry(blueTask1, FINISH, thread1, null));
            
            Assert.False(ScheduleValidator.IsValid(log));
        }
        
        
        

        [SetUp]
        public void InitTest() {
            log = new List<TaskLogEntry>();
        }


        class DummyTask : Task {
            
            public DummyTask(int id, int color) : base(color) {
                ID = id;
            }
            
            public override void Run() {
                // no need to call me
            }
            
        }
        
        
    }
    
}