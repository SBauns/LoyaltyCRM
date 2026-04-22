namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class Image
    {
        public string Value { get; }

        public Image(string Value)
        {
            ValidateImage(Value);
            this.Value = Value;
        }

        private void ValidateImage(string Value)
        {
            // Updated Regex pattern to allow image URLs, including cloud storage URLs like Amazon S3
            string pattern = @"^(https?:\/\/[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(\/[^\s]+)*\.(jpg|jpeg|png|gif|bmp|webp)|data:image\/(jpeg|png|gif|bmp|webp);base64,[a-zA-Z0-9+/]+={0,2})$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(Value, pattern))
            {
                throw new ArgumentException("translation.image.invalid");
            }
        }

    }
}
