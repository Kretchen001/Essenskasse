INSERT INTO Order
    VALUES {
        _id: {"$date": "2023-12-24T14:00:55.2410000Z"},
        Money: {
            Euro: {"$numberLong": "1"},
            Cent: 15
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000000"},
            Designation: "Hoffmanns Hütte"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000007"}
    },
    {
        _id: {"$date": "2023-12-24T14:05:20.1230000Z"},
        Money: {
            Euro: {"$numberLong": "8"},
            Cent: 40
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000001"},
            Designation: "Schmidt's Elektronik"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"}
    },
    {
        _id: {"$date": "2023-12-24T14:10:45.9870000Z"},
        Money: {
            Euro: {"$numberLong": "4"},
            Cent: 80
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000002"},
            Designation: "Müller & Söhne GmbH"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"}
    },
    {
        _id: {"$date": "2023-12-24T14:15:10.3210000Z"},
        Money: {
            Euro: {"$numberLong": "12"},
            Cent: 25
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000003"},
            Designation: "Becker's Baustoffe"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000004"}
    },
    {
        _id: {"$date": "2023-12-24T14:20:35.6540000Z"},
        Money: {
            Euro: {"$numberLong": "6"},
            Cent: 50
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000004"},
            Designation: "Lehmann's Lebensmittel"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000005"}
    },
    {
        _id: {"$date": "2023-12-24T14:25:00.9870000Z"},
        Money: {
            Euro: {"$numberLong": "2"},
            Cent: 90
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000005"},
            Designation: "Wagner's Werkzeuge"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000006"}
    },
    -- Nicht jeder Person muss eine Bestellung aufgegeben haben
    {
        _id: {"$date": "2023-12-24T14:30:15.3210000Z"},
        Money: {
            Euro: {"$numberLong": "10"},
            Cent: 10
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000006"},
            Designation: "Fischer's Fahrzeuge"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000001"}
    },
    {
        _id: {"$date": "2023-12-24T14:35:40.6540000Z"},
        Money: {
            Euro: {"$numberLong": "7"},
            Cent: 30
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000007"},
            Designation: "Lehmann's Lampen"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"}
    },
    {
        _id: {"$date": "2023-12-24T14:40:55.9870000Z"},
        Money: {
            Euro: {"$numberLong": "13"},
            Cent: 70
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000008"},
            Designation: "Lehmann's Lampen"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"}
    },
    {
        _id: {"$date": "2023-12-24T14:45:20.3210000Z"},
        Money: {
            Euro: {"$numberLong": "5"},
            Cent: 40
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000001"},
            Designation: "Schmidt's Elektronik"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000004"}
    },
    {
        _id: {"$date": "2023-12-24T14:50:45.6540000Z"},
        Money: {
            Euro: {"$numberLong": "3"},
            Cent: 99
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000002"},
            Designation: "Müller & Söhne GmbH"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000005"}
    },
    {
        _id: {"$date": "2023-12-24T14:55:10.9870000Z"},
        Money: {
            Euro: {"$numberLong": "9"},
            Cent: 60
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000003"},
            Designation: "Becker's Baustoffe"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000006"}
    },
    {
        _id: {"$date": "2023-12-24T15:00:35.3210000Z"},
        Money: {
            Euro: {"$numberLong": "7"},
            Cent: 10
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000004"},
            Designation: "Lehmann's Lebensmittel"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000007"}
    },
    {
        _id: {"$date": "2023-12-24T15:05:00.6540000Z"},
        Money: {
            Euro: {"$numberLong": "11"},
            Cent: 30
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000005"},
            Designation: "Wagner's Werkzeuge"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000008"}
    },
    {
        _id: {"$date": "2023-12-24T15:10:25.9870000Z"},
        Money: {
            Euro: {"$numberLong": "5"},
            Cent: 80
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000006"},
            Designation: "Fischer's Fahrzeuge"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000001"}
    },
    {
        _id: {"$date": "2023-12-24T15:15:51.3210000Z"},
        Money: {
            Euro: {"$numberLong": "13"},
            Cent: 20
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000007"},
            Designation: "Lehmann's Lampen"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"}
    },
    {
        _id: {"$date": "2023-12-24T15:21:16.6540000Z"},
        Money: {
            Euro: {"$numberLong": "6"},
            Cent: 15
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000001"},
            Designation: "Schmidt's Elektronik"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"}
    },
    {
        _id: {"$date": "2023-12-24T15:26:41.9870000Z"},
        Money: {
            Euro: {"$numberLong": "8"},
            Cent: 50
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000002"},
            Designation: "Müller & Söhne GmbH"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000004"}
    },
    {
        _id: {"$date": "2023-12-24T15:32:07.3210000Z"},
        Money: {
            Euro: {"$numberLong": "10"},
            Cent: 5
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000003"},
            Designation: "Becker's Baustoffe"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000005"}
    },
    {
        _id: {"$date": "2023-12-24T15:37:32.6540000Z"},
        Money: {
            Euro: {"$numberLong": "7"},
            Cent: 90
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000004"},
            Designation: "Lehmann's Lebensmittel"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000006"}
    },
    {
        _id: {"$date": "2023-12-24T15:42:57.9870000Z"},
        Money: {
            Euro: {"$numberLong": "12"},
            Cent: 40
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000005"},
            Designation: "Wagner's Werkzeuge"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000001"}
    },
    {
        _id: {"$date": "2023-12-24T15:48:23.3210000Z"},
        Money: {
            Euro: {"$numberLong": "3"},
            Cent: 70
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000006"},
            Designation: "Fischer's Fahrzeuge"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"}
    },
    {
        _id: {"$date": "2023-12-24T15:53:48.6540000Z"},
        Money: {
            Euro: {"$numberLong": "5"},
            Cent: 40
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000007"},
            Designation: "Lehmann's Lampen"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"}
    },
    {
        _id: {"$date": "2023-12-24T15:59:13.9870000Z"},
        Money: {
            Euro: {"$numberLong": "13"},
            Cent: 10
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000008"},
            Designation: "Fischer's Fahrzeuge"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000004"}
    },
    {
        _id: {"$date": "2023-12-24T16:04:39.3210000Z"},
        Money: {
            Euro: {"$numberLong": "6"},
            Cent: 99
        },
        Provider: {
            _id: {"$guid": "00000000-0000-0000-0000-000000000002"},
            Designation: "Müller & Söhne GmbH"
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000005"}
    };