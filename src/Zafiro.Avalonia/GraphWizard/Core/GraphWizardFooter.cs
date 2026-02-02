using Zafiro.UI.Commands;

namespace Zafiro.Avalonia.Wizards.Graph.Core;

public record GraphWizardFooter(IGraphWizard Wizard, IEnhancedCommand? Cancel = null);