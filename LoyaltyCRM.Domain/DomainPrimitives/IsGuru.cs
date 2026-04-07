namespace PapasCRM_API.DomainPrimitives
{
    public class IsGuru
    {
        private bool value;

        public IsGuru(bool value)
        {
            this.value = value;
        }

        public bool GetValue()
        {
            return this.value;
        }
    }
}
