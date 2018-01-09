namespace clio.Model
{
    public enum ParsingConfidence
    {
        High, // Clearly appears to be a fixed bug
        Low, // A bug, but unclear based on context
        Invalid, // None found
    }
}
