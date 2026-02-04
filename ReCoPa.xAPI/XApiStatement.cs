using System;

namespace ReCoPa.XApi
{
    public class XApiStatement
    {
        // --- Core xAPI concepts (flattened) -------------------

        public string Actor { get; set; }           // "Player X"
        public string Verb { get; set; }             // "interacted", "completed", "looked-at"
        public string ObjectId { get; set; }         // "Task_1", "Object_A"

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // --- Optional semantic hints (VERY useful for live views)

        public string ActivityType { get; set; }     // "task", "object", "ui"
        public string Result { get; set; }            // "success", "fail", null

        // --- XR / ReCoPa specific -----------------------------

        public bool IsGaze { get; set; }
        public bool IsInteraction { get; set; }
        public bool IsTaskRelated { get; set; }

        // Optional numeric value (pulse, duration, score, etc.)
        public double? Value { get; set; }

        // ------------------------------------------------------

        public override string ToString()
            => $"{Actor} {Verb} {ObjectId} @ {Timestamp:HH:mm:ss}";
    }
}