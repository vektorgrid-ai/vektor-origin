using System.Security.Cryptography;
using System.Text;
using AssistantCore.Companion.Dto;

namespace AssistantCore.Companion.Security;

public class RequestValidator
{
    
    public bool VerifyApprovalSignature(
        string base64PublicKey,
        string base64Signature,
        byte[] signedData)
    {
        // Decode inputs
        byte[] publicKeyBytes = Convert.FromBase64String(base64PublicKey);
        byte[] signatureBytes = Convert.FromBase64String(base64Signature);

        // Load public key (X.509 SubjectPublicKeyInfo)
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

        // Verify signature (ASN.1 DER ECDSA)
        return ecdsa.VerifyData(
            signedData,
            signatureBytes,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.Rfc3279DerSequence
        );
    }
    
    public byte[] BuildSignedPayload(ToolApprovalRequest request, ToolAnswer answer)
    {
        string dataToSign = $"{answer.RequestId}|{request.Nonce}|{answer.PayloadHash}|{answer.Timestamp}";
        return Encoding.UTF8.GetBytes(dataToSign);
    }
}