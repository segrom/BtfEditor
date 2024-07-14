namespace btfReader;

 internal class BtfString: IBtfString
{
    public int Id { get; set; } //4B
    public int AddressInFile { get; set; } //4B
    public short Length { get; set; } //2B

    private string _content;
    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            OnChanged?.Invoke();
        }
    } // UTF16BE

    public event Action? OnChanged;
}