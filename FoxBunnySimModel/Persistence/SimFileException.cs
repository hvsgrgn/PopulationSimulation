namespace FoxBunnySimCore.Persistence;

public class SimFileException : Exception{
    public int? LineNumber { get; }

    public SimFileException(string message, int? lineNumber = null, Exception? inner = null)
        : base(lineNumber.HasValue ? $"Line {lineNumber}: {message}" : message, inner){
        LineNumber = lineNumber;
    }
}

public class GameFileAccessException : SimFileException{
    public string Path { get; }

    public GameFileAccessException(string path, string action, Exception inner)
        : base($"Could not {action} the file '{System.IO.Path.GetFileName(path)}': {inner.Message}", null, inner){
        Path = path;
    }
}

public class GameFileEndedEarlyException : SimFileException{
    public string Expected { get; }

    public GameFileEndedEarlyException(int lineNumber, string expected)
        : base($"the file ended, but {expected} was expected.", lineNumber){
        Expected = expected;
    }
}

public class GameFileSyntaxException : SimFileException{
    public string Expected { get; }
    public string Found { get; }

    public GameFileSyntaxException(int lineNumber, string expected, string found)
        : base($"expected {expected}, but found '{found}'.", lineNumber){
        Expected = expected;
        Found = found;
    }
}

public class GameFileValueException : SimFileException{
    public string Field { get; }

    public GameFileValueException(int lineNumber, string field, string reason)
        : base($"invalid {field}: {reason}.", lineNumber){
        Field = field;
    }
}
