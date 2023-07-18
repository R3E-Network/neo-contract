from neo_fairy_client import FairyClient, Hash160Str, Hash256Str, PublicKeyStr

wallet_address = 'NM4cADAcmUjgGZvzz5nqpionpkrmpNCbjb'
publickey = PublicKeyStr('022b22669b3eb1d6eb766b109a58040a5daccf6d6384e06e9d490da329f94c8191')
wallet_scripthash = Hash160Str.from_address(wallet_address)
client = FairyClient(fairy_session='ac', wallet_address_or_scripthash=wallet_scripthash)
client.contract_scripthash = client.virutal_deploy_from_path('../R3E/bin/sc/R3E.nef')
print(client.contract_scripthash)

print(client.find_storage_with_session(''))
ROLE_ADMIN = Hash256Str.zero()
assert client.invokefunction('hasRole', [ROLE_ADMIN, wallet_scripthash])
