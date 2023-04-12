
using Pulumi;
using Pulumi.AzureNative.AzureActiveDirectory;
using System.Threading.Tasks;

class Program
{

    static Task<int> Main()
    {
        return Deployment.RunAsync<B2CStack>();
    }
}