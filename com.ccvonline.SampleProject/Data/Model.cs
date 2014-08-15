using System.Runtime.Serialization;

namespace com.ccvonline.SampleProject.Data
{
    [DataContract]
    public class Model<T> : Rock.Data.Model<T> where T : Rock.Data.Model<T>, Rock.Security.ISecured, new()
    {
    }
}
