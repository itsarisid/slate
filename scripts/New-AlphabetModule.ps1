param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-Za-z][A-Za-z0-9]*$')]
    [string]$Name
)

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$moduleRoot = Join-Path $root "src\Modules\$($Name)Module"

$directories = @(
    'Domain\Entities',
    'Domain\ValueObjects',
    'Domain\Interfaces',
    'Domain\Events',
    'Application\Features',
    'Application\Common\Mappings',
    'Infrastructure\Persistence\Configurations',
    'Infrastructure\Repositories',
    'Api\Models',
    'Api\Resource'
)

foreach ($directory in $directories) {
    New-Item -ItemType Directory -Force -Path (Join-Path $moduleRoot $directory) | Out-Null
}

$registration = @"
using Microsoft.Extensions.DependencyInjection;

namespace Alphabet.Modules.$($Name)Module;

public static class ${Name}ModuleRegistration
{
    public static IServiceCollection Add${Name}Module(this IServiceCollection services)
    {
        return services;
    }
}
"@

Set-Content -Path (Join-Path $moduleRoot "$($Name)ModuleRegistration.cs") -Value $registration -Encoding utf8

Write-Host "Created module scaffold at $moduleRoot"
