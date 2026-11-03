namespace WMS.Models.Response.Audit
{
    public class CtpatCriteriaResponse
    {
        public int IDTopic { get; set; }
        public string Topic { get; set; }
        public string TopicNote { get; set; }
        public List<CtpatCriterion> Criterias { get; set; }
    }
}
