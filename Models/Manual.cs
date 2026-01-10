using System.Collections.Generic;

namespace MyManual.Models.Manual
{
    public class Manual
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;           // 업무명
        public string Category { get; set; } = string.Empty;        // 카테고리 (사내 규칙, 업무도구/시스템 등)
        public string Purpose { get; set; } = string.Empty;         // 업무 목적
        public string Process { get; set; } = string.Empty;         // 프로세스 설명
        public List<ChecklistItem> Checklist { get; set; } = new(); // 체크리스트
        public List<HistoryItem> History { get; set; } = new();     // 히스토리
    }

    public class ChecklistItem
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }

    public class HistoryItem
    {
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
