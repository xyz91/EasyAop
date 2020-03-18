using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
	internal static class CryptoConvert
	{
		private static int ToInt32LE(byte[] bytes, int offset)
		{
			return bytes[offset + 3] << 24 | bytes[offset + 2] << 16 | bytes[offset + 1] << 8 | bytes[offset];
		}

		private static uint ToUInt32LE(byte[] bytes, int offset)
		{
			return (uint)(bytes[offset + 3] << 24 | bytes[offset + 2] << 16 | bytes[offset + 1] << 8 | bytes[offset]);
		}

		private static byte[] Trim(byte[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != 0)
				{
					byte[] array2 = new byte[array.Length - i];
					Buffer.BlockCopy(array, i, array2, 0, array2.Length);
					return array2;
				}
			}
			return null;
		}

		private static RSA FromCapiPrivateKeyBlob(byte[] blob, int offset)
		{
			RSAParameters rSAParameters = default(RSAParameters);
			try
			{
				if (blob[offset] == 7 && blob[offset + 1] == 2 && blob[offset + 2] == 0 && blob[offset + 3] == 0 && ToUInt32LE(blob, offset + 8) == 843141970)
				{
					int num = ToInt32LE(blob, offset + 12);
					byte[] array = new byte[4];
					Buffer.BlockCopy(blob, offset + 16, array, 0, 4);
					Array.Reverse(array);
					rSAParameters.Exponent = Trim(array);
					int num2 = offset + 20;
					int num3 = num >> 3;
					rSAParameters.Modulus = new byte[num3];
					Buffer.BlockCopy(blob, num2, rSAParameters.Modulus, 0, num3);
					Array.Reverse(rSAParameters.Modulus);
					num2 += num3;
					int num4 = num3 >> 1;
					rSAParameters.P = new byte[num4];
					Buffer.BlockCopy(blob, num2, rSAParameters.P, 0, num4);
					Array.Reverse(rSAParameters.P);
					num2 += num4;
					rSAParameters.Q = new byte[num4];
					Buffer.BlockCopy(blob, num2, rSAParameters.Q, 0, num4);
					Array.Reverse(rSAParameters.Q);
					num2 += num4;
					rSAParameters.DP = new byte[num4];
					Buffer.BlockCopy(blob, num2, rSAParameters.DP, 0, num4);
					Array.Reverse(rSAParameters.DP);
					num2 += num4;
					rSAParameters.DQ = new byte[num4];
					Buffer.BlockCopy(blob, num2, rSAParameters.DQ, 0, num4);
					Array.Reverse(rSAParameters.DQ);
					num2 += num4;
					rSAParameters.InverseQ = new byte[num4];
					Buffer.BlockCopy(blob, num2, rSAParameters.InverseQ, 0, num4);
					Array.Reverse(rSAParameters.InverseQ);
					num2 += num4;
					rSAParameters.D = new byte[num3];
					if (num2 + num3 + offset <= blob.Length)
					{
						Buffer.BlockCopy(blob, num2, rSAParameters.D, 0, num3);
						Array.Reverse(rSAParameters.D);
					}
					goto end_IL_0008;
				}
				throw new CryptographicException("Invalid blob header");
				end_IL_0008:;
			}
			catch (Exception inner)
			{
				throw new CryptographicException("Invalid blob.", inner);
			}
			RSA rSA = null;
			try
			{
				rSA = RSA.Create();
				rSA.ImportParameters(rSAParameters);
				return rSA;
			}
			catch (CryptographicException)
			{
				bool flag = false;
				try
				{
					rSA = new RSACryptoServiceProvider(new CspParameters
					{
						Flags = CspProviderFlags.UseMachineKeyStore
					});
					rSA.ImportParameters(rSAParameters);
				}
				catch
				{
					flag = true;
				}
				if (flag)
				{
					throw;
				}
				return rSA;
			}
		}

		private static RSA FromCapiPublicKeyBlob(byte[] blob, int offset)
		{
			try
			{
				if (blob[offset] == 6 && blob[offset + 1] == 2 && blob[offset + 2] == 0 && blob[offset + 3] == 0 && ToUInt32LE(blob, offset + 8) == 826364754)
				{
					int num = ToInt32LE(blob, offset + 12);
					RSAParameters rSAParameters = new RSAParameters
					{
						Exponent = new byte[3]
					};
					rSAParameters.Exponent[0] = blob[offset + 18];
					rSAParameters.Exponent[1] = blob[offset + 17];
					rSAParameters.Exponent[2] = blob[offset + 16];
					int srcOffset = offset + 20;
					int num2 = num >> 3;
					rSAParameters.Modulus = new byte[num2];
					Buffer.BlockCopy(blob, srcOffset, rSAParameters.Modulus, 0, num2);
					Array.Reverse(rSAParameters.Modulus);
					RSA rSA = null;
					try
					{
						rSA = RSA.Create();
						rSA.ImportParameters(rSAParameters);
					}
					catch (CryptographicException)
					{
						rSA = new RSACryptoServiceProvider(new CspParameters
						{
							Flags = CspProviderFlags.UseMachineKeyStore
						});
						rSA.ImportParameters(rSAParameters);
					}
					return rSA;
				}
				throw new CryptographicException("Invalid blob header");
			}
			catch (Exception inner)
			{
				throw new CryptographicException("Invalid blob.", inner);
			}
		}

		public static RSA FromCapiKeyBlob(byte[] blob)
		{
			return FromCapiKeyBlob(blob, 0);
		}

		public static RSA FromCapiKeyBlob(byte[] blob, int offset)
		{
			if (blob == null)
			{
				throw new ArgumentNullException("blob");
			}
			if (offset >= blob.Length)
			{
				throw new ArgumentException("blob is too small.");
			}
			switch (blob[offset])
			{
			case 0:
				if (blob[offset + 12] != 6)
				{
					break;
				}
				return FromCapiPublicKeyBlob(blob, offset + 12);
			case 6:
				return FromCapiPublicKeyBlob(blob, offset);
			case 7:
				return FromCapiPrivateKeyBlob(blob, offset);
			}
			throw new CryptographicException("Unknown blob format.");
		}
	}
}
