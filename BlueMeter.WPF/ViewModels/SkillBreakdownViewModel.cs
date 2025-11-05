using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.ViewModels;

public partial class SkillBreakdownViewModel(ILogger<SkillBreakdownViewModel> logger) : BaseViewModel
{
    [RelayCommand]
    private void Confirm()
    {
        logger.LogDebug("Confirm SkillBreakDown");
    }

    [RelayCommand]
    private void Cancel()
    {
        logger.LogDebug("Cancel SkillBreakDown");
    }
}