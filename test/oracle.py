from neo_fairy_client.rpc.fairy_client import FairyClient, Hash160Str, Hash256Str, GasAddress
import hashlib

ROLE_ADMIN = Hash256Str.zero()
print('ROLE_ORACLE:', ROLE_ORACLE := Hash256Str(hashlib.sha256(b'ORACLE').hexdigest()))

deployer = Hash160Str.from_address('NM4cADAcmUjgGZvzz5nqpionpkrmpNCbjb')
deployer_WIF_private_key = 'L3v6oswfiUGUuE6HR66VH7KF2A7ipQzBjbHcRC31cbEa5REH5qcY'
# deployer_private_key: bytes = binascii.hexlify(base58.b58decode(deployer_WIF_private_key))[2:-8]
# print('DEPLOYER PRIVATE KEY:', deployer_private_key)

client = FairyClient(fairy_session='oracle', wallet_address_or_scripthash=deployer)
client.set_session_fairy_wallet_with_WIF(deployer_WIF_private_key, "passwd")
client.contract_scripthash = client.virutal_deploy_from_path('../R3E/bin/sc/R3E.nef')
print('ORACLE:', client.contract_scripthash, client.contract_scripthash.to_address())
dice_contract = client.virutal_deploy_from_path('../example/bin/sc/example.nef', auto_set_client_contract_scripthash=False)
print('DICE:', dice_contract)

result = client.invokefunction_of_any_contract(dice_contract, 'play', [deployer, []])
print(result)
print(tx := client.previous_raw_result['result']['tx'])
