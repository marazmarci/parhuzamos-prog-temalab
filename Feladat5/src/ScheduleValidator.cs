using System;
using System.Collections.Generic;
using System.Threading;
using static TaskLogEntryType;

namespace Feladat5 {
    
    public class ScheduleValidator {
        
        public static bool IsValid(List<TaskLogEntry> log) {
            
            var colors = new HashSet<int>();
            var runningColors = new HashSet<int>();
            var taskToThreadMap = new Dictionary<Task, Thread>();
            var failedTasks = new HashSet<Task>();
            var failedTaskToLogEntryMap = new Dictionary<Task,TaskLogEntry>();

            bool rule1ok = true;
            bool rule2ok = true;
            bool rule3ok = true;
            bool rule4ok = true;


            foreach (var entry in log) {
                var color = entry.Color;
                var type = entry.Type;
                var task = entry.Task;
                var thread = entry.Thread;

                colors.Add(color);


                // CHECK RULE #1:

                const string rule1 = "Egy feladatot csak egy végrehajtónak szabad kiadni.";

                switch (type) {
                    case START:
                        if (taskToThreadMap.TryGetValue(task, out var _)) {
                            rule1ok = false;
                            PrintBadSchedule(entry, rule1);
                        }
                        else {
                            taskToThreadMap.Add(task, thread);
                        }
                        break;
                    case FINISH:
                        if (taskToThreadMap.TryGetValue(task, out var starterThread)) {
                            var finisherThread = thread;
                            if (starterThread != finisherThread) {
                                rule1ok = false;
                                PrintBadSchedule(entry, rule1);
                            }
                        }
                        else {
                            rule1ok = false;
                            PrintBadSchedule(entry, "task finished without started!!?!");
                        }
                        break;
                    case FAIL:
                        if (taskToThreadMap.TryGetValue(task, out starterThread)) {
                            var failedThread = thread;
                            if (starterThread != failedThread) {
                                rule1ok = false;
                                PrintBadSchedule(entry, rule1);
                            }
                        } else {
                            rule1ok = false;
                            PrintBadSchedule(entry, rule1);
                        }
                        taskToThreadMap.Remove(task);
                        break;
                }


                // CHECK RULE #2
                // error message is printed at the end

                switch (type) {
                    case FAIL:
                        failedTasks.Add(task);
                        failedTaskToLogEntryMap.Add(task, entry);
                        break;
                    case START:
                        failedTasks.Remove(task);
                        failedTaskToLogEntryMap.Remove(task);
                        break;
                }
                


                // CHECK RULE #3:

                const string rule3 = "Az egész rendszerben egy adott színű feladatból egy időben csak egy lehet végrehajtás alatt.";

                if (runningColors.Contains(color)) {
                    switch (type) {
                        case START:
                            // not ok
                            rule3ok = false;
                            PrintBadSchedule(entry, rule3);
                            break;
                        case FINISH:
                        case FAIL:
                            // ok
                            runningColors.Remove(color);
                            break;
                    }
                }
                else {
                    switch (type) {
                        case START:
                            // ok
                            runningColors.Add(color);
                            break;
                        case FINISH:
                        case FAIL:
                            // not ok
                            rule3ok = false;
                            PrintBadSchedule(entry, rule3);
                            break;
                    }
                }
            }


            // CHECK RULE #4:

            const string rule4 = "Egy adott színhez tartozó feladatokat a beadás sorrendjében kell végrehajtani.";

            foreach (var color in colors) {
                int lastFinishId = -1;
                int lastStartId = -1;
                int prevLastStartId = -1;
                //var failedTasks = new HashSet<Task>();
                foreach (var entry in log) {
                    if (entry.Color == color) {
                        var id = entry.ID;
                        var task = entry.Task;

                        switch (entry.Type) {
                            case START:
                                if (id <= lastStartId) {
                                    // not ok
                                    rule4ok = false;
                                    PrintBadSchedule(entry, rule4);
                                }
                                prevLastStartId = lastStartId;
                                lastStartId = id;
                                break;
                            case FINISH:
                                if (id <= lastFinishId) {
                                    // not ok
                                    rule4ok = false;
                                    PrintBadSchedule(entry, rule4);
                                }
                                lastFinishId = id;
                                break;
                            case FAIL:
                                lastStartId = prevLastStartId;
                                break;
                        }
                    }
                }
            }
            
            // CHECK RULE #2
            
            const string rule2 = "Ha a feladat végrehajtása valamilyen okból meghiúsul (pl. a végrehajtó szál elesik), a feladatot vissza kell tenni a végrehajtási sorba.";

            if (failedTasks.Count != 0) {
                foreach (var task in failedTasks) {
                    var entry = failedTaskToLogEntryMap[task];
                    PrintBadSchedule(entry, rule2);
                }
                rule2ok = false;
            }
            

            return rule1ok && rule2ok && rule3ok && rule4ok;
        }
        
        private static void PrintBadSchedule(TaskLogEntry entry, string brokenRule) {
            Console.WriteLine($"Bad schedule: {entry}  \"{brokenRule}\"");
        }
        
    }
    
}