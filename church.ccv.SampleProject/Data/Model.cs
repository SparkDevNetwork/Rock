using System.Runtime.Serialization;

namespace church.ccv.SampleProject.Data
{
    [DataContract]
    public abstract class Model<T> : Rock.Data.Model<T> where T : Rock.Data.Model<T>, Rock.Security.ISecured, new()
    {
    }
}
