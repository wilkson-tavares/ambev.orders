import { useQuery } from '@tanstack/react-query'
import { ordersApi } from '../api/orders'
import { StatusBadge } from './StatusBadge'
import type { OrderStatus } from '../types/order'
import { Eye } from 'lucide-react'

interface Props {
  status: OrderStatus
  onSelectOrder: (id: number) => void
}

export function OrderList({ status, onSelectOrder }: Props) {
  const { data: orders, isLoading, isError, refetch } = useQuery({
    queryKey: ['orders', status],
    queryFn: () => ordersApi.listByStatus(status),
  })

  if (isLoading) return <p className="text-sm text-gray-500">Carregando pedidos...</p>

  if (isError)
    return (
      <div className="text-center py-6">
        <p className="text-sm text-red-500 mb-2">Erro ao carregar pedidos.</p>
        <button onClick={() => refetch()} className="text-xs text-violet-600 underline">
          Tentar novamente
        </button>
      </div>
    )

  if (!orders?.length)
    return <p className="py-8 text-center text-sm text-gray-400">Nenhum pedido com status "{status}".</p>

  return (
    <div className="overflow-hidden rounded-lg border border-gray-200">
      <table className="w-full text-sm">
        <thead className="bg-gray-50">
          <tr className="border-b text-left text-xs text-gray-500">
            <th className="px-4 py-2.5">ID Externo</th>
            <th className="px-4 py-2.5">ID Interno</th>
            <th className="px-4 py-2.5">Cliente</th>
            <th className="px-4 py-2.5 text-right">Imposto</th>
            <th className="px-4 py-2.5">Status</th>
            <th className="px-4 py-2.5" />
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => (
            <tr key={order.id} className="border-b last:border-0 hover:bg-gray-50/50">
              <td className="px-4 py-3 font-medium">#{order.orderId}</td>
              <td className="px-4 py-3 text-gray-500">{order.id}</td>
              <td className="px-4 py-3">#{order.clientId}</td>
              <td className="px-4 py-3 text-right font-medium text-violet-700">
                {order.tax.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
              </td>
              <td className="px-4 py-3">
                <StatusBadge status={order.status as OrderStatus} />
              </td>
              <td className="px-4 py-3 text-right">
                <button
                  onClick={() => onSelectOrder(order.id)}
                  className="flex items-center gap-1 rounded border border-gray-200 px-2 py-1 text-xs text-gray-600 hover:border-violet-400 hover:text-violet-600"
                >
                  <Eye size={12} /> Detalhes
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
