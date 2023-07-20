from neo_fairy_client.rpc.fairy_client import FairyClient, Hash160Str, PublicKeyStr, GasAddress

wallet_address = 'NM4cADAcmUjgGZvzz5nqpionpkrmpNCbjb'
wallet_scripthash = Hash160Str.from_address(wallet_address)
publickey = PublicKeyStr('022b22669b3eb1d6eb766b109a58040a5daccf6d6384e06e9d490da329f94c8191')

forwardRequest = [
    wallet_scripthash,
    publickey,
    GasAddress,
    1000_0000_0000,
    1231,
    'transfer',
    [wallet_scripthash, wallet_scripthash, 0, None]
]

client = FairyClient(fairy_session='verifySig', wallet_address_or_scripthash=wallet_scripthash)
client.virutal_deploy_from_path('../R3E/bin/sc/R3E.nef')
print(client.contract_scripthash)
client.delete_source_code_breakpoints()
client.set_source_code_breakpoint('MinimalForwarder.cs', 48)
breakpoint_ = client.debug_function_with_session('verifySig', [forwardRequest, bytearray.fromhex("6a45ede22926143ba4d7703084f2a35ef9ce0383917ab414f5ab2942dac8a59a901add388a7b9a23af834687160abe1e09e77e7faf8e86e386140bcc302a32fb")])
print(breakpoint_)
# print('signature', client.get_variable_value_by_name('signature'))
print('req', client.get_variable_value_by_name('req'))
print('network', client.get_variable_value_by_name('network'))
# print('concat', client.get_variable_value_by_name('concat'))
print('fakeScript', client.get_variable_value_by_name('fakeScript'))
print('fakeTransaction', client.get_variable_value_by_name('fakeTransaction'))
print('msg', client.get_variable_value_by_name('msg'))
# print(client.debug_step_into())

assert client.invokefunction('verifySig', [forwardRequest, bytearray.fromhex("6a45ede22926143ba4d7703084f2a35ef9ce0383917ab414f5ab2942dac8a59a901add388a7b9a23af834687160abe1e09e77e7faf8e86e386140bcc302a32fb")]) is True
