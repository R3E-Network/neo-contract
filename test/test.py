from neo_fairy_client import FairyClient, Hash160Str, Hash256Str, PublicKeyStr
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
print('call data, or req.data:', '0x' + c.invokefunction('callDataToVerify', [forward_request]).hex())
print(msg_to_sign := c.invokefunction('dataToVerify', [forward_request]))
msg_to_sign = msg_to_sign.to_UInt256()

'''
// Open your browser with metamask plugin
// visit https://metamask.github.io/test-dapp
// launch a local hardhat node with `npx hardhat node`. It should have chain Id 31337 with JSON-RPC server at http://127.0.0.1:8545/
// import ethereum wallet with the private key of variable deployer_private_key_hex
// the address should be 0x1feB0bD36208145A40dC0A2004e31f094ecFa7E7
// execute the following codes in the console of developer tools (F12)
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
        "chainId": 31337,
        "verifyingContract": "0x5FbDB2315678afecb367f032d93F642f64180aa3"
      },
      "message": {
          from: '0x7651a826505c19ca20326381e96b0d5439519a0c',
          to: '0x37909f174bac07bcf9e5eaedd2158bc46c578e30',
          value: 0,
          gas: 1000_0000,
          nonce: 1234,
          data: '0x40022804706c6179400228140c9a5139540d6be981633220ca195c5026a85176210101'
      }
    }
  ]
});
'''
auto_signature = sk.sign(msg_to_sign)
print('auto_signature:', auto_signature.hex())
signature = input("Input metamask signature result, or leave this empty and use auto sigs: ")
# signature = ""
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

'''Tests to reproduce ethereum cases'''
c.with_print = False
# print()
test_struct = [Hash160Str('0x06b9931e101f2e9885e76503874f2cebf69bf790'), PublicKeyStr.from_ecdsa_verifying_key(vk), Hash160Str('0x02660107ede44e33f5281d7148311917d63a3f66'), 0, 300_0000, 1, 'play', [deployer, 1]]
test_calldata = b'\x93\xe8\x4c\xd9'
test_struct_to_verify: bytes = c.invokefunction('structToVerify', [test_struct, test_calldata])
assert test_struct_to_verify.hex() == "dd8f4b70b0f4393e889bd39128a30628a78b61816a9eb8199759e7a349657e4800000000000000000000000090f79bf6eb2c4f870365e785982e1f101e93b906000000000000000000000000663f3ad617193148711d28f5334ee4ed07016602000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002dc6c0000000000000000000000000000000000000000000000000000000000000000103c3502d86f21966b48c4c9d22b70395d32b485c6026b345a28d1cb2793d0d89"
assert c.invokefunction('mINIMAL_FORWARDER_TYPE_HASH') == Hash256Str('0x487e6549a3e7599719b89e6a81618ba72806a32891d39b883e39f4b0704b8fdd')
# print()
assert c.invokefunction('dOMAIN_SEPARATOR_TYPE_HASH') == Hash256Str('0x0f40392b525da7a9cafa0f9b177b9f2379cc59f74ccc2e513dfeb89bc6c3738b')
# print('hashedName', c.invokefunction('hashedName'))
assert c.invokefunction('hashedName') == Hash256Str('0xdc084b50acbdea561bf8b67a3f8c4b7ebf9a4b69fbc9eb8c9a5e519fa323099e')
assert c.invokefunction('hashedVersion') == Hash256Str('0x851881639119fc8fbdacd9794864879330cf325d45f28042051cf2480b9a20ae')
assert c.invokefunction('domainSeparatorV4TestAbi').hex() == '8b73c3c69bb8fe3d512ecc4cf759cc79239f7b179b0ffacaa9a75d522b39400f9e0923a39f515e9a8cebc9fb694b9abf7e4b8c3f7ab6f81b56eabdac504b08dcae209a0b48f21c054280f2455d32cf309387644879d9acbd8ffc1991638118850000000000000000000000000000000000000000000000000000000000007a690000000000000000000000005fbdb2315678afecb367f032d93f642f64180aa3'
assert c.invokefunction('domainSeparatorV4') == Hash256Str('0xc7d403ce35603ce8c77b96d1dddddabea40459424cb63e33295e2624b71aa3e7')
assert c.invokefunction('keccak256', [test_struct_to_verify]) == Hash256Str('0x99ab9f1256cfe9972c33fa05cd1374d4fe0ae119b051c546ea98e6ad61c50b8f'), 'keccak256(test_struct)'
assert c.invokefunction('dataToVerify', [test_struct, test_calldata]) == Hash256Str('0xbfee5b8b095f81d5f79e947a003791e074a727de60feacab557c1501564e328b'), 'dataToVerify'
# print(c.invokefunction('abiencode', [Hash160Str('0x06b9931e101f2e9885e76503874f2cebf69bf790'), True]))
# c.delete_source_code_breakpoints()
# c.delete_assembly_breakpoints()
# c.set_assembly_breakpoints(161)
# print(c.debug_function_with_session('abiencode', [Hash160Str('0x06b9931e101f2e9885e76503874f2cebf69bf790'), True]))
# breakpoint()
