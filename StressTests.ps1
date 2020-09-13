
workflow workflow1{


    Param($NumberofIterations)
    "======================================================="
    $array = 1..$NumberofIterations

	
    function DoRequest($i,$Uri){
		$Uri = "https://13.80.152.17/api/sessions/d7a3998f-020b-4a40-a6a7-817634318ef4/submit"
		$data = ConvertTo-Json @('chocolate-protein-shake-100072', 'strawberry-protein-shake-100072', 'vanilla-protein-shake-100074')
	add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
	[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
        "$i starting";$response = Invoke-WebRequest -Uri $Uri -ContentType "application/json" -Method POST -Body $data;"$i ending"
    }

    "CountingStrings Stress Tests"
    "========"
    $startTime = get-date
    foreach -parallel ($i in $array) {DoRequest $i $Uri}
    $parallelElapsedTime = "elapsed time (parallel foreach loop): " + ((get-date) - $startTime).TotalSeconds
    $serialElapsedTime
    $parallelElapsedTime
    "======================================================="
}
cls
workflow1 10000
	



