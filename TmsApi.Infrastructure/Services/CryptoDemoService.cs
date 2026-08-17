

public class CryptoDemoService
{
    public string HashUserPassword(string plainText)
    {
        return Bcrypt.Net.Bcrypt.HashPassword(plainText, workFactor:12)
        
    }
    
}