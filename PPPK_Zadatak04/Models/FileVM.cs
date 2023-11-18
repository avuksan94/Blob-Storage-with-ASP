namespace PPPK_Zadatak04.Models
{
    public class FileVM
    {
        public string? Extension { get; set; }
        public List<string> Files { get; set; } = new();
    }
}
