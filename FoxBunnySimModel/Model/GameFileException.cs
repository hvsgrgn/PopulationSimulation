namespace FoxBunnySimCore.Model;

//the view only knows this one, the errors of the persistence layer are wrapped into it
public class GameFileException : Exception{
    public GameFileException(string message, Exception inner) : base(message, inner){}
}
