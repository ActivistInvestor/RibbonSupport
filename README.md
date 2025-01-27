All code from this repo has been merged into AcMgdLib
at https://github.com/ActivistInvestor/AcMgdLib

RibbonExtensionApplication has been deprecated
in favor of RibbonEventManager.

The reasons arise largely out of the fact that some who need 
the functionality provided by RibbonExtensionApplication already 
have IExtensionApplications that derive from another custom base 
type, which immensely-complicates integrating the functionality
provided by RibbonExtensionApplication, which is also an abstract
base type that must be derived from to use.

Instead, RibbonEventManager offers the equivalent functionality
without having to derive an IExtensionApplication from another
base type, allowing the functionality to be integrated into any
existing IExtensionApplication with minimal effort.

RibbonEventManager now also supports enabling the querying
of ICommand.CanExecute() for all ribbon item command handlers
when commands or macros start/end, and when the active document 
changes.
