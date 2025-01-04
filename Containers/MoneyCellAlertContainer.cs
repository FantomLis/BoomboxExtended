using System.Collections.Generic;
using System.Linq;

namespace FantomLis.BoomboxExtended.Containers;

public record MoneyCellAlertContainer(string Header, MoneyCellUI.MoneyCellType AlertType, params string [] description)
{
    public string Header = Header;
    public MoneyCellUI.MoneyCellType AlertType = AlertType;
    public List<string> Description = description.ToList();
    private string [] description = description;
}