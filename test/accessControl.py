from neo_fairy_client import FairyClient, Hash160Str, Hash256Str, PublicKeyStr, Signer

wallet_address = 'NM4cADAcmUjgGZvzz5nqpionpkrmpNCbjb'
publickey = PublicKeyStr('022b22669b3eb1d6eb766b109a58040a5daccf6d6384e06e9d490da329f94c8191')
deployer = Hash160Str.from_address(wallet_address)
melon_eater = Hash160Str.from_address('Nb2CHYY5wTh2ac58mTue5S3wpG6bQv5hSY')
power_seizer = Hash160Str.from_address('NaainHz563mJLsHRsPD4NrKjMEQGBXXJY9')
client = FairyClient(fairy_session='ac', wallet_address_or_scripthash=deployer)
client.contract_scripthash = client.virutal_deploy_from_path('../R3E/bin/sc/R3E.nef')
print(client.contract_scripthash)

print(client.find_storage_with_session(''))
ROLE_ADMIN = Hash256Str.zero()
assert client.invokefunction('hasRole', [ROLE_ADMIN, deployer]) is True
client.invokefunction('grantRole', [ROLE_ADMIN, melon_eater, deployer])
assert client.invokefunction('hasRole', [ROLE_ADMIN, melon_eater]) is True
client.invokefunction('grantRole', [ROLE_ADMIN, power_seizer, melon_eater], signers=Signer(melon_eater))
client.invokefunction('revokeRole', [ROLE_ADMIN, deployer, power_seizer], signers=Signer(power_seizer))
assert client.invokefunction('hasRole', [ROLE_ADMIN, deployer]) is False
assert 'is missing role' in client.invokefunction('revokeRole', [ROLE_ADMIN, power_seizer, deployer], do_not_raise_on_result=True)
client.invokefunction('renounceRole', [ROLE_ADMIN, deployer])
client.invokefunction('revokeRole', [ROLE_ADMIN, melon_eater, melon_eater], signers=Signer(melon_eater))
assert client.invokefunction('hasRole', [ROLE_ADMIN, deployer]) is False
assert client.invokefunction('hasRole', [ROLE_ADMIN, melon_eater]) is False
assert client.invokefunction('hasRole', [ROLE_ADMIN, power_seizer]) is True
client.invokefunction('renounceRole', [ROLE_ADMIN, power_seizer], signers=Signer(power_seizer))
# warning: nobody has admin power now
print(client.find_storage_with_session(''))