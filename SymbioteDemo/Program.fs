module Progam

open System
open Symbiote.Core
open Symbiote.Relax
open Symbiote.Daemon

type PersonCouchDocument =
    val id : string
    val mutable name : string
    val mutable address : string
    inherit DefaultCouchDocument 
        new (id, name, address) = 
            {id = id; name = name; address = address} then base.DocumentId <- id
        member x.Name
            with get () = x.name
        member x.Address
            with get () = x.address

type DemoService = 
    val couchServer : ICouchServer
    new(couchServer) = {couchServer = couchServer}
    interface IDaemon with
        member x.Start () =
            do Console.WriteLine("The service has started")
            let document = new PersonCouchDocument("123456", "John Doe", "123 Main")
            x.couchServer.Repository.Save(document)
            do Console.WriteLine(
                "The document with name {0} and address {1} was saved successfully", 
                document.Name, document.Address)
            let documentRetrieved = 
                x.couchServer.Repository.Get<PersonCouchDocument>(document.DocumentId);
            do Console.WriteLine(
                "The document with name {0} and address {1} was retrieved successfully", 
                documentRetrieved.Name, documentRetrieved.Address)
            do x.couchServer.DeleteDatabase<PersonCouchDocument>()
        member x.Stop () =
            do Console.WriteLine("The service has stopped")

do Assimilate
    .Core()
    .Daemon<DemoService>(fun x -> (x.Name("FSharpDemoService")
                                    .DisplayName("An FSharp Demo Service")
                                    .Description("An FSharp Demo Service")
                                    .Arguments([||]) |> ignore))
    .Relax(fun x -> x.Server("localhost") |> ignore) 
    .RunDaemon<DemoService>() 
