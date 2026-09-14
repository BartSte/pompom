using Pompom.Core;

namespace Pompom.Services;

internal interface ISettingsStore
{
    PompomSettings Load();

    void Save(PompomSettings settings);
}
