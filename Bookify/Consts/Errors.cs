namespace Bookify.Consts
{
    public class Errors
    {

        public const string MaxLength = "Length cannot be more than {1} Characters";
        public const string Duplicated = "{0} with the Same name is already Exists!";
        public const string BookDuplicated = "Book with the Same Title is already Exists with the same author!";
        public const string allowedDate = "Publishing Date can not be in the future!";
       
        public const string NotAllowedExtensions = "only .png , .jpg , .jpeg  Files are allowed!";
        public const string MaxSize = "file can not be more than 2MB!";

    }
}
