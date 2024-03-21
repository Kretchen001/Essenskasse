DROP COLLECTION Person;
DROP COLLECTION Provider;
DROP COLLECTION Payment;
DROP COLLECTION Order;

INSERT INTO Person
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000001"},
        Name: "Max Mustermann",
        Email: "max@example.com",
        Saldo: {
            "Euro":{"$numberLong":"0"},
            "Cent":0
        }
    };

INSERT INTO Person
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000002"},
        Name: "Anna Schmidt",
        Email: "anna@example.com",
        Saldo: {
            "Euro":{"$numberLong":"0"},
            "Cent":0
        }
    };
    
INSERT INTO Person
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000003"},
        Name: "Peter Müller",
        Email: "peter@example.com",
        Saldo: {
            "Euro":{"$numberLong":"0"},
            "Cent":0
        }
    };

INSERT INTO Person
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000004"},
        Name: "Julia Fischer",
        Email: "julia@example.com",
        Saldo: {
            "Euro":{"$numberLong":"0"},
            "Cent":0
        }
    };
    
INSERT INTO Person
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000005"},
        Name: "Markus Schneider",
        Email: "markus@example.com",
        Saldo: {
            "Euro":{"$numberLong":"0"},
            "Cent":0
        }
    };

INSERT INTO Person
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000006"},
        Name: "Laura Wagner",
        Email: "laura@example.com",
        Saldo: {
            "Euro":{"$numberLong":"0"},
            "Cent":0
        }
    };

INSERT INTO Person
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000007"},
        Name: "Andreas Becker",
        Email: "andreas@example.com",
        Saldo: {
            "Euro":{"$numberLong":"0"},
            "Cent":0
        }
    };

INSERT INTO Person
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000008"},
        Name: "Sabine Lehmann",
        Email: "sabine@example.com",
        Saldo: {
            "Euro":{"$numberLong":"0"},
            "Cent":0
        }
    };

INSERT INTO Provider
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000000"},
        Designation: "Hoffmanns Hütte"
    };
    
INSERT INTO Provider
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000001"},
        Designation: "Schmidt's Elektronik"
    };

INSERT INTO Provider
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000002"},
        Designation: "Müller & Söhne GmbH"
    };

INSERT INTO Provider
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000003"},
        Designation: "Becker's Baustoffe"
    };

INSERT INTO Provider
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000004"},
        Designation: "Lehmann's Lebensmittel"
    };
    
INSERT INTO Provider
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000005"},
        Designation: "Wagner's Werkzeuge"
    };

INSERT INTO Provider
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000006"},
        Designation: "Fischer's Fahrzeuge"
    };

INSERT INTO Provider
    VALUES {
        _id: {"$guid": "00000000-0000-0000-0000-000000000007"},
        Designation: "Lehmann's Lampen"
    };

INSERT INTO Payment
    VALUES {
        _id: {"$date": "2023-12-25T15:13:44.6510000Z"},
        Amount: {
            Euro: {"$numberLong": "10"},
            Cent: 0
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000001"},
        Comment: "Lorem ipsum dolor sit amet consectetur adipisicing elit. Molestias aut, repellat ipsum facere voluptate dicta obcaecati deserunt nobis suscipit eaque?"
    },
    {
        _id: {"$date": "2023-12-25T15:14:22.1230000Z"},
        Amount: {
            Euro: {"$numberLong": "20"},
            Cent: 50
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"},
        Comment: "Dolor sit amet."
    },
    {
        _id: {"$date": "2023-12-25T15:15:00.9870000Z"},
        Amount: {
            Euro: {"$numberLong": "15"},
            Cent: 75
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:16:30.4560000Z"},
        Amount: {
            Euro: {"$numberLong": "25"},
            Cent: 20
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:17:15.7890000Z"},
        Amount: {
            Euro: {"$numberLong": "12"},
            Cent: 30
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000004"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:18:04.3210000Z"},
        Amount: {
            Euro: {"$numberLong": "18"},
            Cent: 90
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000006"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:18:11.7890000Z"},
        Amount: {
            Euro: {"$numberLong": "30"},
            Cent: 40
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000001"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:19:45.3210000Z"},
        Amount: {
            Euro: {"$numberLong": "18"},
            Cent: 90
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:20:30.1230000Z"},
        Amount: {
            Euro: {"$numberLong": "22"},
            Cent: 60
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:21:15.9870000Z"},
        Amount: {
            Euro: {"$numberLong": "27"},
            Cent: 15
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000004"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:22:00.4560000Z"},
        Amount: {
            Euro: {"$numberLong": "14"},
            Cent: 80
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000005"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:22:45.7890000Z"},
        Amount: {
            Euro: {"$numberLong": "32"},
            Cent: 50
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000006"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:23:30.3210000Z"},
        Amount: {
            Euro: {"$numberLong": "19"},
            Cent: 10
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000007"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:24:15.6540000Z"},
        Amount: {
            Euro: {"$numberLong": "28"},
            Cent: 35
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000008"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:25:00.9870000Z"},
        Amount: {
            Euro: {"$numberLong": "15"},
            Cent: 60
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000001"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:25:45.3210000Z"},
        Amount: {
            Euro: {"$numberLong": "22"},
            Cent: 30
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:26:30.6540000Z"},
        Amount: {
            Euro: {"$numberLong": "18"},
            Cent: 45
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:27:15.9870000Z"},
        Amount: {
            Euro: {"$numberLong": "13"},
            Cent: 20
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000004"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:28:00.3210000Z"},
        Amount: {
            Euro: {"$numberLong": "17"},
            Cent: 80
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000005"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:28:45.6540000Z"},
        Amount: {
            Euro: {"$numberLong": "26"},
            Cent: 50
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000006"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:29:30.9870000Z"},
        Amount: {
            Euro: {"$numberLong": "8"},
            Cent: 40
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:30:15.3210000Z"},
        Amount: {
            Euro: {"$numberLong": "6"},
            Cent: 90
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000008"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:31:00.6540000Z"},
        Amount: {
            Euro: {"$numberLong": "9"},
            Cent: 20
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000001"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:31:45.9870000Z"},
        Amount: {
            Euro: {"$numberLong": "7"},
            Cent: 50
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000002"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:32:30.3210000Z"},
        Amount: {
            Euro: {"$numberLong": "5"},
            Cent: 10
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:33:15.6540000Z"},
        Amount: {
            Euro: {"$numberLong": "8"},
            Cent: 80
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000004"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:34:00.9870000Z"},
        Amount: {
            Euro: {"$numberLong": "6"},
            Cent: 30
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000005"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:34:45.3210000Z"},
        Amount: {
            Euro: {"$numberLong": "9"},
            Cent: 60
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000006"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:35:30.6540000Z"},
        Amount: {
            Euro: {"$numberLong": "7"},
            Cent: 20
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000003"},
        Comment: ""
    },
    {
        _id: {"$date": "2023-12-25T15:36:15.9870000Z"},
        Amount: {
            Euro: {"$numberLong": "5"},
            Cent: 50
        },
        ReferencedPerson: {"$guid": "00000000-0000-0000-0000-000000000008"},
        Comment: ""
    };

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