namespace Common
{
    using System.ComponentModel.DataAnnotations;

    public class DummyData
    {
        public DummyData() : this(string.Empty, string.Empty)
        {
        }

        public DummyData(string id, string data)
        {
            this.Id = id;
            this.Data = data;
        }

        [Required]
        public string Id { get; set; }

        [StringLength(100)]
        public string Data { get; set; }
    }
}