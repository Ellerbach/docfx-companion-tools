using System.Collections.Generic;
using DocFXLanguageGenerator.Helpers;

namespace DocLanguageTranslator.Test.Helpers;

internal class MockMessageHelper : IMessageHelper
{
    public List<string> Errors { get; } = new List<string>();
    public List<string> Warnings { get; } = new List<string>();
    public List<string> VerboseMessages { get; } = new List<string>();

    public void Error(string message)
        => this.Errors.Add(message);

    public void Verbose(string message)
        => this.VerboseMessages.Add(message);

    public void Warning(string message)
        => this.Warnings.Add(message);
}
