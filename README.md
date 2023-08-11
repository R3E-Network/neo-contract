# r3e neo contract

## 1. Contract Integration

### 1.1 oracle integration

1. develop your Neo N3 contract as usual except for the functions need the r3e oracle
2. develop the functions that needs the r3e oracle, these functions must have and only have two parameters
    * user account, this account's witness will be checked by r3e, you should not check it again
    * your other arguments in an array, you can have any amount of args here
3. in the oracle related functions, you could fetch the oracle data by calling R3E's contract
    `OraclePayload data = (OraclePayload)Contract.Call(R3E, "data", CallFlags.ReadOnly, ${YOUR_REGISTERED_KEY})`

### 1.2 oracle data

```dotnet
public struct OraclePayload
{
    public UInt256 hashkey;
    public object data;
    public ulong timestamp;
}
```

oracle data are stored at the R3E contract, indexed by the key you registered at initial stage, it has three fields, the first is for validation check, the second is the data itself, the third is for expiration check

### 1.3 security

1. check the function is directly called by R3E
2. after fetching the data from oracle, **check** the key and timestamp is valid
3. **do not call other contracts** including transfer NEP-17 and NEP-11 assets, if you need to give the account a prize, it's prefered to record the prize first, then let the user claim it in another function

### 1.4 example

* [Dice](./example/Dice.cs) is a simple example that relies on R3E's random seed
* structure
    1. declared the `Win` event and `OraclePayload` struct first, 
    2. then the constant members (including the random data's key and the MOD as well as the R3E's address) are declared
    3. the core function `Play` fetch the data and do the security checks, then issue the event if condition reached

## 2. Build && Test R3E

### 2.1 dependency

* [NEO Devpack Dotnet](https://github.com/neo-project/neo-devpack-dotnet)
* [NEO fairy test](https://github.com/Hecate2/neo-fairy-test)
* [NEO fairy client](https://github.com/Hecate2/neo-fairy-client)

### 2.2 build R3E contract

* `dotnet new -i Neo3.SmartContract.Templates`
* `cd R3E && dotnet build`, you will see the contract's manifest and nef file at `R3E/bin/sc`

### 2.3 test

Firstly, setup https://github.com/Hecate2/neo-fairy-test/ at your local machine, for neo **mainnet**. No need to wait for block syncing.

```bash
cd test
pip install -r requirements.txt
python accessControl.py
python oracle.py
python verifySig.py
```

