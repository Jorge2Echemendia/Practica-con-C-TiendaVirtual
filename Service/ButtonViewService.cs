using MudBlazor;
namespace TiendaVirtual.Service;

public class ButtonViewService
{
    public bool IsVisible { get; private set; } = false;
    public string Icon => IsVisible ? Icons.Material.Filled.Visibility : Icons.Material.Filled.VisibilityOff;
    public InputType InputType => IsVisible ? InputType.Text : InputType.Password;

    public void Toggle()
    {
        IsVisible = !IsVisible;
    }
}
