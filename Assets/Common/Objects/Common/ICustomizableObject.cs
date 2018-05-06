namespace APlusOrFail.Objects
{
    public interface ICustomizableObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        /// <returns>true if any rect has changed</returns>
        bool NextSetting(int option);
    }
}
