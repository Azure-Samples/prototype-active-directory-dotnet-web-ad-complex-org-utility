#MSOnline\Connect-MsolService

$domain="[affiliate domain name matching on-prem UPN suffix]"
$customStsFQDN = "[Custom STS FQDN]"

#[string]$cert="[Base-64 encoded string representing the public key of your STS cert]"

#this is the cert string corresponding to the sample/test cert "LocalSTS.pfx", located in "ComplexOrgSTS/App_Data"
#DO NOT USE IN PRODUCTION
[string]$cert="MIIB6zCCAVigAwIBAgIQi1q8MJwYe4xEYjoJne/rfTAJBgUrDgMCHQUAMBMxETAPBgNVBAMTCExvY2FsU1RTMB4XDTEyMDExMjIyMjA0MFoXDTM5MTIzMTIzNTk1OVowEzERMA8GA1UEAxMITG9jYWxTVFMwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBALClI6MgLV5EdnrZwcmsO+N8YoCgK4jWpncn2YxUU9cjCcYn1ks/9aSSJuFv9S3U6jalyN42jcCcP9/IIZngDMO0Rrdhyj+ra2AqP3Wj3oo6nHSpAmL+U32AZuxtvCLBsruik0OzKYkQshzdFLTthvIu7+jInanAmjn2T6Det8ehAgMBAAGjSDBGMEQGA1UdAQQ9MDuAEERfdZTmD4dsGFoHguIUVCahFTATMREwDwYDVQQDEwhMb2NhbFNUU4IQi1q8MJwYe4xEYjoJne/rfTAJBgUrDgMCHQUAA4GBAIdIbWFAVqq28keKyp6/UPOUxO3j2WsSxMm7yiePDhZVkaqLoq2QqySaHv3tvLA9GTRsd8E1RLSEZ7yZUVgv3J3n3GpD6RVcwxx9Dw1gEes7zZdq5KqnpgBqOEbUR1CEZa8hGswXbYN0Jve1+yqCObq1bfqcluHCWmhP9Fw9x1li"

$url="https://$customStsFQDN/sts"
$issuerUri="http://$customStsFQDN/sts/$domain/services/trust"
$logouturl="https://$customStsFQDN/sts"

$settings = MSOnline\Get-MsolDomainFederationSettings -DomainName $domain

MSOnline\Set-MsolDomainAuthentication `
    -Authentication Federated `
    -DomainName $domain `
    -SigningCertificate $cert `
    -FederationBrandName $domain `
    -ActiveLogOnUri "$($url)/trust" `
    -MetadataExchangeUri "$($url)/trust/mex" `
    -PassiveLogOnUri $url `
    -IssuerUri $issuerUri `
    -LogOffUri $logouturl `
    -PreferredAuthenticationProtocol WsFed

$settings2 = MSOnline\Get-MsolDomainFederationSettings -DomainName $domain

"Original:"
$settings

""
"New:"
$settings2

#Set-MsolDomainAuthentication -DomainName $domain -Authentication Managed
