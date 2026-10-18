using CommunityToolkit.Mvvm.Input;
using RouteOuverte.Models;

namespace RouteOuverte.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}