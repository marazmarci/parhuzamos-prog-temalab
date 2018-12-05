using System;
using System.Collections.Generic;
using System.Threading;

namespace Feladat5 {
    
    public class ScheduleValidator {
        
        public static bool Validate(List<TaskLogEntry> log) {
            var colors = new HashSet<int>();
            var runningColors = new HashSet<int>();
            var taskToThreadMap = new Dictionary<Task, Thread>();

            bool rule1ok = true;
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
                    case TaskLogEntryType.START:
                        if (taskToThreadMap.TryGetValue(task, out var _)) {
                            rule1ok = false;
                            PrintBadSchedule(entry, rule1);
                        }
                        else {
                            taskToThreadMap.Add(task, thread);
                        }
                        break;
                    case TaskLogEntryType.FINISH:
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
                }


                // CHECK RULE #3:

                const string rule3 =
                    "Az egész rendszerben egy adott színű feladatból egy időben csak egy lehet végrehajtás alatt.";

                if (runningColors.Contains(color)) {
                    switch (type) {
                        case TaskLogEntryType.START:
                            // not ok
                            rule3ok = false;
                            PrintBadSchedule(entry, rule3);
                            break;
                        case TaskLogEntryType.FINISH:
                            // ok
                            runningColors.Remove(color);
                            break;
                    }
                }
                else {
                    switch (type) {
                        case TaskLogEntryType.START:
                            // ok
                            runningColors.Add(color);
                            break;
                        case TaskLogEntryType.FINISH:
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
                foreach (var entry in log) {
                    if (entry.Color == color) {
                        var id = entry.ID;

                        switch (entry.Type) {
                            case TaskLogEntryType.START:
                                if (id <= lastStartId) {
                                    // not ok
                                    rule4ok = false;
                                    PrintBadSchedule(entry, rule4);
                                }
                                lastStartId = id;
                                break;
                            case TaskLogEntryType.FINISH:
                                if (id <= lastFinishId) {
                                    // not ok
                                    rule4ok = false;
                                    PrintBadSchedule(entry, rule4);
                                }
                                lastFinishId = id;
                                break;
                        }
                    }
                }
            }

            return rule1ok && rule3ok && rule4ok;
        }
        
        private static void PrintBadSchedule(TaskLogEntry entry, string brokenRule) {
            Console.WriteLine($"Bad schedule: {entry}  \"{brokenRule}\"");
        }
        
    }
    
}