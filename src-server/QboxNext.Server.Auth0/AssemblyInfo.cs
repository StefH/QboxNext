using System.Runtime.CompilerServices;
using RestEase;

// This is needed because the IAuth0TokenApi, IAuth0UserApi and IAuth0Api are marked as 'internal'.
[assembly: InternalsVisibleTo(RestClient.FactoryAssemblyName)]