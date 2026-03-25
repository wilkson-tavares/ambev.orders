import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '../api/orders'
import { StatusBadge } from './StatusBadge'
import { ArrowLeft, Play, Send } from 'lucide-react'
import type { OrderStatus } from '../types/order'

interface Props {
  orderId: number
  onBack: () => void
}

export function OrderDetail({ orderId, onBack }: Props) {
  const queryClient = useQueryClient()

  const { data: order, isLoading, isError } = useQuery({
    queryKey: ['order', orderId],
    queryFn: () => ordersApi.getById(orderId),
  })

  const startProcessing = useMutation({
    mutationFn: () => ordersApi.startProcessing(orderId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['order', orderId] })
      queryClient.invalidateQueries({ queryKey: ['orders'] })
    },
  })

  const send = useMutation({
    mutationFn: () => ordersApi.send(orderId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['order', orderId] })
      queryClient.invalidateQueries({ queryKey: ['orders'] })
    },
  })

  const actionError =
    (startProcessing.error as { response?: { data?: { error?: string } } })?.response?.data?.error ||
    (send.error as { response?: { data?: { error?: string } } })?.response?.data?.error

  if (isLoading) return <p className="text-sm text-gray-500">Carregando...</p>
  if (isError || !order) return <p className="text-sm text-red-500">Pedido não encontrado.</p>

  const totalItems = order.items.reduce((acc, i) => acc + i.value * i.quantity, 0)

  return (
    <div className="space-y-5">
      <button onClick={onBack} className="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-800">
        <ArrowLeft size={14} /> Voltar
      </button>

      <div className="rounded-lg border border-gray-200 p-5 space-y-4">
        <div className="flex items-start justify-between">
          <div>
            <h2 className="text-lg font-semibold text-gray-800">Pedido #{order.orderId}</h2>
            <p className="text-sm text-gray-500">Cliente #{order.clientId} · ID interno: {order.id}</p>
          </div>
          <StatusBadge status={order.status as OrderStatus} />
        </div>

        <div className="grid grid-cols-2 gap-3 rounded-lg bg-gray-50 p-4 text-sm">
          <div>
            <p className="text-gray-500">Total dos itens</p>
            <p className="font-semibold text-gray-800">
              {totalItems.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
            </p>
          </div>
          <div>
            <p className="text-gray-500">Imposto calculado</p>
            <p className="font-semibold text-violet-700">
              {order.tax.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
            </p>
          </div>
        </div>

        <div>
          <h3 className="mb-2 text-sm font-medium text-gray-700">Itens ({order.items.length})</h3>
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-left text-xs text-gray-500">
                <th className="pb-1">Produto</th>
                <th className="pb-1 text-right">Qtd</th>
                <th className="pb-1 text-right">Valor unit.</th>
                <th className="pb-1 text-right">Subtotal</th>
              </tr>
            </thead>
            <tbody>
              {order.items.map((item, i) => (
                <tr key={i} className="border-b last:border-0">
                  <td className="py-1.5">#{item.productId}</td>
                  <td className="py-1.5 text-right">{item.quantity}</td>
                  <td className="py-1.5 text-right">
                    {item.value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </td>
                  <td className="py-1.5 text-right font-medium">
                    {(item.value * item.quantity).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {actionError && (
          <p className="rounded bg-red-50 px-3 py-2 text-sm text-red-600">{actionError}</p>
        )}

        <div className="flex gap-2">
          {order.status === 'Created' && (
            <button
              onClick={() => startProcessing.mutate()}
              disabled={startProcessing.isPending}
              className="flex items-center gap-1.5 rounded bg-yellow-500 px-4 py-2 text-sm font-medium text-white hover:bg-yellow-600 disabled:opacity-60"
            >
              <Play size={14} /> {startProcessing.isPending ? 'Aguarde...' : 'Iniciar Processamento'}
            </button>
          )}
          {order.status === 'Processing' && (
            <button
              onClick={() => send.mutate()}
              disabled={send.isPending}
              className="flex items-center gap-1.5 rounded bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-60"
            >
              <Send size={14} /> {send.isPending ? 'Enviando...' : 'Enviar para Sistema B'}
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
