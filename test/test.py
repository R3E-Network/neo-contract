from neo_fairy_client import FairyClient, Hash160Str, PublicKeyStr
import ecdsa, base58, hashlib

deployer = Hash160Str.from_address('NM4cADAcmUjgGZvzz5nqpionpkrmpNCbjb')
deployer_publickey = PublicKeyStr('022b22669b3eb1d6eb766b109a58040a5daccf6d6384e06e9d490da329f94c8191')
deployer_WIF_private_key = 'L3v6oswfiUGUuE6HR66VH7KF2A7ipQzBjbHcRC31cbEa5REH5qcY'
deployer_private_key_hex = 'c7cd74e8e38e992611ec457b431b47470007391d1f7e11cc705374688904f5f5'
deployer_ethereum_mainnet_address = '0x1feB0bD36208145A40dC0A2004e31f094ecFa7E7'

c = FairyClient(fairy_session='abi-encode', wallet_address_or_scripthash=deployer)
# keccak_address = c.virutal_deploy_from_path(nef_path_and_filename='../neo-keccak256/Keccak256Contract/bin/sc/Keccak256.nef')
print(minimal_forwarder_address := c.virutal_deploy_from_path(nef_path_and_filename='../R3E/bin/sc/MinimalForwarder.nef'))
print(minimal_forwarder_address.to_address(), minimal_forwarder_address)
c.contract_scripthash = minimal_forwarder_address
print(c.invokefunction('keccak256', ["ForwardRequest(address from,address to,uint256 value,uint256 gas,uint256 nonce,bytes data)"]))
# 0x487e6549a3e7599719b89e6a81618ba72806a32891d39b883e39f4b0704b8fdd
print(c.invokefunction('domainSeparatorV4'))

print(dice_address := c.virutal_deploy_from_path('../example/bin/sc/example.nef', auto_set_client_contract_scripthash=False))

deployer_private_key_bytes = base58.b58decode(deployer_WIF_private_key)[1:-5]
sk = ecdsa.SigningKey.from_string(deployer_private_key_bytes, curve=ecdsa.SECP256k1, hashfunc=hashlib.sha256)
vk = sk.get_verifying_key()

forward_request = [deployer, PublicKeyStr.from_ecdsa_verifying_key(vk), dice_address, 0, 1000_0000, 1234, 'play', [deployer, 1]]
print(msg_to_sign := c.invokefunction('dataToVerify', [forward_request]))
msg_to_sign = msg_to_sign.to_UInt256()

'''
const accounts = await ethereum.request({
    method: 'eth_requestAccounts',
  });
await window.ethereum.request({
  "method": "eth_signTypedData_v4",
  "params": [
    "0x1feB0bD36208145A40dC0A2004e31f094ecFa7E7",
    {
      "types": {
        "EIP712Domain": [
          {
            "name": "name",
            "type": "string"
          },
          {
            "name": "version",
            "type": "string"
          },
          {
            "name": "chainId",
            "type": "uint256"
          },
          {
            "name": "verifyingContract",
            "type": "address"
          }
        ],
        "ForwardRequest": [
            { name: 'from', type: 'address' },
            { name: 'to', type: 'address' },
            { name: 'value', type: 'uint256' },
            { name: 'gas', type: 'uint256' },
            { name: 'nonce', type: 'uint256' },
            { name: 'data', type: 'bytes' }
        ]
      },
      "primaryType": "ForwardRequest",
      "domain": {
        "name": "MinimalForwarder",
        "version": "0.0.1",
        "chainId": 1,
        "verifyingContract": "0xa810b1b57aa73341b77353ad94cad08c2437e311"
      },
      "message": {
          from: '0x7651a826505c19ca20326381e96b0d5439519a0c',
          to: '0x94b39c096338bc15f6023d83617b1660ee9f7fb4',
          value: 0,
          gas: 1000_0000,
          nonce: 1234,
          data: '0x39136c1d2d587d56d4440e7a57c14f35fb80960b6dc3730e29c5d4ab25d909a8'
      }
    }
  ]
});
'''
auto_signature = sk.sign(msg_to_sign)
print(auto_signature.hex())
# signature = input("Input metamask signature result, or leave this empty and use auto sigs: ")
signature = ""
if signature:
    signature = signature.replace("0x", "").replace(r'"', "")
    signature = bytes.fromhex(signature)
    if len(signature) == 65:
        signature = signature[:-1]
    assert len(signature) == 64, "Invalid signature"
else:
    signature = auto_signature
assert vk.verify(signature, msg_to_sign)

oracle_payloads = [[c.invokefunction_of_any_contract(dice_address, 'hashKey'), b'rua!!', c.get_time_milliseconds() + 60_000],]
c.invokefunction('grantOracleRole', [deployer, deployer])
c.invokefunction('executeWithData', [oracle_payloads, forward_request, signature])
