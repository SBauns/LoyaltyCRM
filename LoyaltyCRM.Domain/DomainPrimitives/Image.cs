namespace LoyaltyCRM.Domain.DomainPrimitives
{
    public class Image
    {
        private string value;

        public Image(string value)
        {
            ValidateImage(value);
            this.value = value;
        }

        public string GetValue()
        {
            return this.value;
        }

        private void ValidateImage(string value)
        {
            // Updated Regex pattern to allow image URLs, including cloud storage URLs like Amazon S3
            string pattern = @"^(https?:\/\/[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(\/[^\s]+)*\.(jpg|jpeg|png|gif|bmp|webp)|data:image\/(jpeg|png|gif|bmp|webp);base64,[a-zA-Z0-9+/]+={0,2})$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
            {
                throw new ArgumentException("Invalid image format. Only valid URLs or base64 encoded images are allowed.");
            }
        }

    }
}
