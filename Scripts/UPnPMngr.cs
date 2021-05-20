using System;
using System.Threading;
using System.Threading.Tasks;
using Open.Nat;

public static class UPnPMngr
{

    public static async Task ForwardPort(int forwardedPort, bool useTcp = false){

        Protocol chosenProtocol = useTcp? Protocol.Tcp: Protocol.Udp;

        var discoverer = new NatDiscoverer();
        //try for 10 seconds to find a nat device
        var cts = new CancellationTokenSource(10000);
        NatDevice device = null;
        try{ device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts); }
        catch(Exception e){ Console.WriteLine("no NAT device was found"); return; }
            //when a nat device is found print the exernal IP and create a port map
            var ip = await device.GetExternalIPAsync();
            Console.WriteLine("The external IP Address is: {0} ", ip);
            var mappings = await device.GetAllMappingsAsync();
            Mapping first = null;
            foreach(var mapping in mappings){
                if(first == null) first = mapping;
                break;
                // Console.WriteLine(mapping);
            }
            //remove an old entry if one exists
            await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, forwardedPort, forwardedPort, "cool map name"));
            await device.DeletePortMapAsync(new Mapping(Protocol.Udp, forwardedPort, forwardedPort, "cool map name"));
            //attempt to make a new port mapping
            try{ await device.CreatePortMapAsync(new Mapping(chosenProtocol, forwardedPort, forwardedPort, "cool map name")); }
            catch(MappingException e){
                if(e.ErrorCode != 501) throw e;

                //replace the first entry if the map table is full
                await device.DeletePortMapAsync(first);
                await device.CreatePortMapAsync(new Mapping(chosenProtocol, forwardedPort, forwardedPort, "cool map name"));
                Console.WriteLine("the NAT table was full so the first entry was replaced");
            }
    }
}
