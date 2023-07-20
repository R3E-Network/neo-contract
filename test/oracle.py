from neo_fairy_client import FairyClient, Hash160Str, Hash256Str, PublicKeyStr
import hashlib
import base64, base58, binascii
import ecdsa

ROLE_ADMIN = Hash256Str.zero()
# print('ROLE_ORACLE:', ROLE_ORACLE := Hash256Str(hashlib.sha256(b'ORACLE').hexdigest()))
print('DICE_HASHKEY:', DICE_HASHKEY := Hash256Str(hashlib.sha256(b'RANDOM').hexdigest()))

deployer = Hash160Str.from_address('NM4cADAcmUjgGZvzz5nqpionpkrmpNCbjb')
deployer_publickey = PublicKeyStr('022b22669b3eb1d6eb766b109a58040a5daccf6d6384e06e9d490da329f94c8191')
deployer_WIF_private_key = 'L3v6oswfiUGUuE6HR66VH7KF2A7ipQzBjbHcRC31cbEa5REH5qcY'
# deployer_private_key: bytes = binascii.hexlify(base58.b58decode(deployer_WIF_private_key))[2:-8]
# print('DEPLOYER PRIVATE KEY:', deployer_private_key)

client = FairyClient(fairy_session='oracle', wallet_address_or_scripthash=deployer)
client.set_session_fairy_wallet_with_WIF(deployer_WIF_private_key, "passwd")
client.contract_scripthash = client.virutal_deploy_from_path('../R3E/bin/sc/R3E.nef')
print('R3E:', client.contract_scripthash, client.contract_scripthash.to_address())
client.invokefunction('grantOracleRole', [deployer, deployer])

dice_contract = client.virutal_deploy_from_path('../example/bin/sc/example.nef', auto_set_client_contract_scripthash=False)
print('DICE:', dice_contract)

# result = client.invokefunction_of_any_contract(dice_contract, 'play', [deployer, []], do_not_raise_on_result=True)
# print(result)
# print(script := client.previous_raw_result['result']['script'])
fakeScript_byte: bytes = client.invokefunction('serializeHelper', [[dice_contract, 'play', [deployer, []]]])
print('fakeScript:', fakeScript_byte.hex())
tx_base64_encoded = client.force_sign_transaction(script_base64_encoded=base64.b64encode(fakeScript_byte), nonce=1231, system_fee=1000_0000, valid_until_block=0)
print(tx_base64_encoded)
print(full_tx := client.previous_raw_result)
oracle_payloads = [[DICE_HASHKEY, None, client.get_time_milliseconds() + 60_000],]
forward_request = [deployer, deployer_publickey, dice_contract, full_tx['gasconsumed'], full_tx['nonce'], "play", [deployer, []]]

msg_to_sign = client.invokefunction('msgHelper', [forward_request])
print('Message to sign:', msg_to_sign)
deployer_private_key_bytes = base58.b58decode(deployer_WIF_private_key)[1:-5]
sk = ecdsa.SigningKey.from_string(deployer_private_key_bytes, curve=ecdsa.NIST256p, hashfunc=hashlib.sha256)
vk = sk.get_verifying_key()
signature = sk.sign(msg_to_sign)
assert vk.verify(signature, msg_to_sign)

print(client.invokefunction("executeWithData", [oracle_payloads, forward_request, signature]))
