## Demo Configuration


Needed:

* The Complex Org Utility
* Three unique AD environments
  * (1) MasterHQ
  * (1) LocalADOnly
  * (1) AADB2B

The utility installation is detailed in the [Setup](../Docs/Setup.md) doc. 
To establish a lab environment for this system, you can leverage the Azure Active
Directory Hybrid ADFS Lab:

https://github.com/Azure-Samples/active-directory-lab-hybrid-adfs

 Run that deployment three times, one each for each of the environments listed above. It
 is recommended that you setup four distinct Resource Groups in Azure, one for the 
 Complex Org Utility and one each for the three AD environments.
