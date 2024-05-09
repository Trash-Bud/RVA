
    using System.Collections.Generic;
    using DefaultNamespace;

    public static class CaptureInfo
    {
        static CaptureInfo()
        {
            capturedTargets = new HashSet<Target>();
        }
        public static Target PlayerTarget { get; set; }
        public static Target EnemyTarget { get; set; }
        
        public static bool battleEnded { get; set; }
        
        public static bool PlayerWon { get; set; }

        public static HashSet<Target> capturedTargets { get; set; }

    }
